import type { Envelope, PaginationEnvelope } from "@/shared/api";
import type {
	InfiniteData,
	QueryClient,
	QueryKey,
} from "@tanstack/react-query";
import { departmentsApi } from "./api";
import type { ChangeDepartmentActivityRequest } from "./types";
import type { DepartmentId } from "../model/types";

type DepartmentCacheItem = {
	id: DepartmentId;
	isActive: boolean;
};

type DepartmentQueryData = InfiniteData<
	Envelope<PaginationEnvelope<DepartmentCacheItem>>
>;

export type DepartmentQueriesSnapshot = [
	QueryKey,
	DepartmentQueryData | undefined,
][];

export function optimisticallyUpdateDepartmentActivity(
	queryClient: QueryClient,
	request: ChangeDepartmentActivityRequest,
): DepartmentQueriesSnapshot {
	const snapshots = queryClient.getQueriesData<DepartmentQueryData>({
		queryKey: [departmentsApi.baseKey],
	});

	for (const [queryKey, data] of snapshots) {
		queryClient.setQueryData(
			queryKey,
			updateDepartmentQueryData(data, queryKey, request),
		);
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

function updateDepartmentQueryData(
	data: DepartmentQueryData | undefined,
	queryKey: QueryKey,
	request: ChangeDepartmentActivityRequest,
): DepartmentQueryData | undefined {
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
						department.id === request.departmentId
							? { ...department, isActive: request.isActive }
							: department,
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
