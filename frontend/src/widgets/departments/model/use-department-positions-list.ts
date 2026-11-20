import { EnvelopeError } from "@/shared/api/errors";
import { useCursorRef } from "@/shared/hooks/use-cursor-ref";
import { useInfiniteQuery } from "@tanstack/react-query";
import { useDebounce } from "use-debounce";
import {
	positionsApi,
	type GetPositionsInfinityRequest,
} from "@/entities/positions";
import {
	usePositionIsActive,
	usePositionSearch,
	usePositionSortBy,
	usePositionSortDirection,
	type PositionListId,
} from "@/features/positions";
import type { DepartmentId } from "@/entities/departments";

type Props = {
	positionsDepartmentId: DepartmentId | null;
	stateId?: PositionListId;
	request?: GetPositionsInfinityRequest;
};

export function useDepartmentPositionsList({
	request,
	stateId,
	positionsDepartmentId,
}: Props) {
	const search = usePositionSearch(stateId);
	const [debouncedSearch] = useDebounce(search, 600);
	const isActive = usePositionIsActive(stateId);
	const sortBy = usePositionSortBy(stateId);
	const sortDirection = usePositionSortDirection(stateId);

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
			departmentIds: positionsDepartmentId
				? [positionsDepartmentId]
				: undefined,
			search: debouncedSearch,
			isActive: isActive === "all" ? undefined : isActive === "active",
			sortBy,
			sortDirection,
		}),
		enabled: positionsDepartmentId !== null,
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
