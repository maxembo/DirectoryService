import { type Envelope, type PaginationEnvelope } from "@/shared/api";
import { InfiniteData, QueryClient, QueryKey } from "@tanstack/react-query";
import { departmentsApi } from "./api";
import type { ChangeDepartmentActivityRequest } from "./types";
import type { DepartmentId } from "../model/types";

type DepartmentCacheItem = {
	id: DepartmentId;
	isActive: boolean;
};

type DepartmentListQueryData = InfiniteData<
	Envelope<PaginationEnvelope<DepartmentCacheItem>>
>;

type DepartmentDetailQueryData = Envelope<DepartmentCacheItem>;

export type DepartmentQueriesSnapshot = [
	QueryKey,
	DepartmentQueryData | undefined,
][];

type DepartmentQueryData = DepartmentListQueryData | DepartmentDetailQueryData;

export function optimisticallyUpdateDepartmentActivity(
	queryClient: QueryClient,
	request: ChangeDepartmentActivityRequest,
): DepartmentQueriesSnapshot {
	const snapshots = queryClient.getQueriesData<DepartmentQueryData>({
		queryKey: [departmentsApi.baseKey],
	});

	for (const [queryKey, data] of snapshots) {
		if (!data) continue;

		if ("pages" in data) {
			queryClient.setQueryData(
				queryKey,
				updateDepartmentListQueryData(data, queryKey, request),
			);
		} else {
			queryClient.setQueryData(
				queryKey,
				updateDepartmentDetailData(data, request),
			);
		}
	}

	return snapshots;
}

export function restoreDepartmentQueries(
	queryClient: QueryClient,
	snapshots: DepartmentQueriesSnapshot,
) {
	for (const [queryKey, data] of snapshots) {
		queryClient.setQueryData(queryKey, data);
	}
}

function updateDepartmentListQueryData(
	data: DepartmentListQueryData | undefined,
	queryKey: QueryKey,
	request: ChangeDepartmentActivityRequest,
): DepartmentListQueryData | undefined {
	if (!data) return data;

	const containsDepartment = data.pages.some((page) =>
		page.result?.items.some(
			(department) => department.id === request.departmentId,
		),
	);
	if (!containsDepartment) return data;

	const removeFromQuery = doesActivityFilterExclude(queryKey, request.isActive);

	return {
		...data,
		pages: data.pages.map((page) => {
			if (!page.result) return page;

			const items = removeFromQuery
				? page.result.items.filter(
						(department) => department.id !== request.departmentId,
					)
				: page.result.items.map((department) =>
						updateDepartmentActivity(department, request),
					);

			return {
				...page,
				result: {
					...page.result,
					items,
					totalCount: removeFromQuery
						? Math.max(0, page.result.totalCount - 1)
						: page.result.totalCount,
				},
			};
		}),
	};
}

function updateDepartmentDetailData(
	data: DepartmentDetailQueryData | undefined,
	request: ChangeDepartmentActivityRequest,
): DepartmentDetailQueryData | undefined {
	if (!data || !data.result) return data;

	return { ...data, result: updateDepartmentActivity(data.result, request) };
}

function doesActivityFilterExclude(
	queryKey: QueryKey,
	isActive: boolean,
): boolean {
	const request = queryKey[2];
	if (!request || typeof request !== "object") return false;

	if ("onlyActive" in request && request.onlyActive === true) {
		return !isActive;
	}

	return (
		"isActive" in request &&
		typeof request.isActive === "boolean" &&
		request.isActive !== isActive
	);
}

function updateDepartmentActivity<T extends DepartmentCacheItem>(
	department: T,
	request: ChangeDepartmentActivityRequest,
): T {
	if (department.id !== request.departmentId) {
		return department;
	}

	return { ...department, isActive: request.isActive };
}
