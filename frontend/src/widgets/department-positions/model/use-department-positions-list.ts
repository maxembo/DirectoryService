import { positionsApi } from "@/entities/positions/api/api";
import { GetPositionsInfinityRequest } from "@/entities/positions/api/types";
import {
	DepartmentTreeId,
	useDepartmentTreeSelectedId,
} from "@/features/departments/department-tree/model/department-tree-store";
import { EnvelopeError } from "@/shared/api/errors";
import { useCursorRef } from "@/shared/hooks/use-cursor-ref";
import { useInfiniteQuery } from "@tanstack/react-query";
import { useDebounce } from "use-debounce";
import {
	PositionListId,
	usePositionIsActive,
	usePositionSearch,
	usePositionSortBy,
	usePositionSortDirection,
} from "../../../features/positions/model/position-list-store";

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
	};
}
