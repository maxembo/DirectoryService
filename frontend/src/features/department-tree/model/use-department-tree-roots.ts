import {
	departmentsApi,
	type GetDepartmentTreeRootsRequest,
} from "@/entities/departments";
import { useCursorRef } from "@/shared/hooks";
import { useInfiniteQuery } from "@tanstack/react-query";
import {
	useDepartmentTreeOnlyActive,
	type DepartmentTreeId,
} from "./department-tree-store";

type Props = {
	request?: GetDepartmentTreeRootsRequest;
	stateId?: DepartmentTreeId;
};

export function useDepartmentTreeRoots({ request, stateId }: Props) {
	const onlyActive = useDepartmentTreeOnlyActive(stateId);

	const {
		data,
		isPending,
		isError,
		error,
		fetchNextPage,
		hasNextPage,
		isFetchingNextPage,
		isFetchNextPageError,
		refetch,
	} = useInfiniteQuery({
		...departmentsApi.getDepartmentTreeRootsInfinityQueryOptions({
			...request,
			onlyActive,
		}),
	});

	const cursorRef = useCursorRef({
		fetchNextPage,
		hasNextPage,
		isFetchingNextPage,
	});
	return {
		departmentRoots: data?.result?.items ?? [],
		isPending,
		isError,
		error,
		cursorRef,
		isFetchingNextPage,
		isFetchNextPageError,
		fetchNextPage,
		refetch,
	};
}
