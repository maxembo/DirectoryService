import {
	departmentsApi,
	type GetDepartmentChildrenRequest,
} from "@/entities/departments";
import { EnvelopeError } from "@/shared/api";
import { useInfiniteQuery } from "@tanstack/react-query";
import {
	type DepartmentTreeId,
	useDepartmentTreeOnlyActive,
} from "./department-tree-store";

type Props = {
	request: GetDepartmentChildrenRequest;
	enabled: boolean;
	stateId?: DepartmentTreeId;
};

export function useInfiniteDepartmentChildren({
	request,
	enabled,
	stateId,
}: Props) {
	const onlyActive = useDepartmentTreeOnlyActive(stateId);

	const {
		data,
		isLoading,
		isError,
		error,
		isFetchingNextPage,
		isFetchNextPageError,
		hasNextPage,
		fetchNextPage,
		refetch,
	} = useInfiniteQuery({
		...departmentsApi.getDepartmentChildrenInfinityOptions({
			...request,
			onlyActive,
		}),
		enabled: enabled,
	});

	return {
		departmentChildren: data?.result?.items ?? [],
		isLoading,
		isError,
		errorMessage:
			error instanceof EnvelopeError
				? error.message
				: "Не удалось загрузить подразделения",
		isFetchingNextPage,
		isFetchNextPageError,
		hasNextPage,
		fetchNextPage,
		refetch,
	};
}
