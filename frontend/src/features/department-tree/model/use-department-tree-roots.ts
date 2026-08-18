import {
	departmentsApi,
	type GetDepartmentTreeRootsRequest,
} from "@/entities/departments";
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
		isFetchNextPageError,
		refetch,
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
		isFetchNextPageError,
		fetchNextPage,
		refetch,
	};
}
