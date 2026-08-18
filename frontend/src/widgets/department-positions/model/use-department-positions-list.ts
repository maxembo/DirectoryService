import { EnvelopeError } from "@/shared/api/errors";
import { useCursorRef } from "@/shared/hooks/use-cursor-ref";
import { useInfiniteQuery } from "@tanstack/react-query";
import { useDebounce } from "use-debounce";
import {
	positionsApi,
	type GetPositionsInfinityRequest,
} from "@/entities/positions";
import {
	useDepartmentTreeSelectedId,
	type DepartmentTreeId,
} from "@/features/department-tree";
import {
	usePositionIsActive,
	usePositionSearch,
	usePositionSortBy,
	usePositionSortDirection,
	type PositionListId,
} from "@/features/positions";

type Props = {
	stateId?: PositionListId;
	departmentTreeStateId?: DepartmentTreeId;
	request?: GetPositionsInfinityRequest;
};

export function useDepartmentPositionsList({
	request,
	stateId,
	departmentTreeStateId,
}: Props) {
	const search = usePositionSearch(stateId);
	const [debouncedSearch] = useDebounce(search, 600);
	const isActive = usePositionIsActive(stateId);
	const sortBy = usePositionSortBy(stateId);
	const sortDirection = usePositionSortDirection(stateId);
	const selectedDepartmentId = useDepartmentTreeSelectedId(
		departmentTreeStateId,
	);

	const {
		data,
		isPending,
		isError,
		error,
		isFetchingNextPage,
		fetchNextPage,
		hasNextPage,
		refetch,
	} = useInfiniteQuery({
		...positionsApi.getPositionsInfiniteQueryOptions({
			...request,
			departmentIds: selectedDepartmentId ? [selectedDepartmentId] : undefined,
			search: debouncedSearch,
			isActive: isActive === "all" ? undefined : isActive === "active",
			sortBy,
			sortDirection,
		}),
		enabled: selectedDepartmentId !== null,
	});

	const cursorRef = useCursorRef({
		hasNextPage: hasNextPage,
		isFetchingNextPage,
		fetchNextPage,
	});

	return {
		positions: data?.result?.items ?? [],
		isPending,
		isError,
		error: error instanceof EnvelopeError ? error : undefined,
		isFetchingNextPage,
		fetchNextPage,
		cursorRef,
		refetch,
	};
}
