import { departmentsApi } from "@/entities/departments/api/api";
import { GetDepartmentTreeRootsRequest } from "@/entities/departments/api/types";
import { useCursorRef } from "@/shared/hooks/use-cursor-ref";
import { useInfiniteQuery } from "@tanstack/react-query";

type Props = {
	request?: GetDepartmentTreeRootsRequest;
};

export function useDepartmentTreeRoots({ request }: Props) {
	const {
		data,
		isPending,
		isError,
		error,
		fetchNextPage,
		hasNextPage,
		isFetchingNextPage,
	} = useInfiniteQuery({
		...departmentsApi.getDepartmentTreeRootsInfinityQueryOptions({
			...request,
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
	};
}
