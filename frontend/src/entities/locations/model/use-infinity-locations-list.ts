import { locationsApi } from "@/entities/locations/api/api";
import { GetLocationsInfinityRequest } from "@/entities/locations/api/types";
import {
	LocationListId,
	useLocationIsActive,
	useLocationSearch,
	useLocationSelectedDepartments,
	useLocationSortBy,
	useLocationSortDirection,
} from "@/features/locations/model/location-list-store";
import { EnvelopeError } from "@/shared/api/errors";
import { useCursorRef } from "@/shared/hooks/use-cursor-ref";
import { useInfiniteQuery as useInfinityQuery } from "@tanstack/react-query";
import { useDebounce } from "use-debounce";

type Props = {
	stateId?: LocationListId;
	request?: GetLocationsInfinityRequest;
};

export function useInfinityLocationsList({ stateId, request }: Props) {
	const selectedDepartments = useLocationSelectedDepartments(stateId);
	const search = useLocationSearch(stateId);
	const [debouncedSearch] = useDebounce(search, 600);
	const isActive = useLocationIsActive(stateId);
	const sortBy = useLocationSortBy(stateId);
	const sortDirection = useLocationSortDirection(stateId);

	const {
		data,
		isPending,
		isError,
		error,
		hasNextPage,
		isFetchingNextPage,
		fetchNextPage,
	} = useInfinityQuery({
		...locationsApi.getLocationsInfinityQueryOptions({
			departmentIds: selectedDepartments.map((department) => department.id),
			search: debouncedSearch,
			isActive: isActive === "all" ? undefined : isActive === "active",
			sortBy,
			sortDirection,
			...request,
		}),
	});

	const cursorRef = useCursorRef({
		hasNextPage,
		isFetchingNextPage,
		fetchNextPage,
	});

	return {
		locations: data?.result?.items ?? [],
		isPending: isPending,
		isError: isError,
		error: error instanceof EnvelopeError ? error : undefined,
		isFetchingNextPage,
		cursorRef: cursorRef,
		hasNextPage,
	};
}
