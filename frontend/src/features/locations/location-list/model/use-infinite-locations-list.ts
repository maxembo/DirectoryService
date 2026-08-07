import { locationsApi } from "@/entities/locations/api/api";
import { GetLocationsInfinityRequest } from "@/entities/locations/api/types";
import {
	LocationListId,
	useLocationSearch,
	useLocationSelectedDepartments,
	useLocationSortBy,
	useLocationSortDirection,
} from "@/features/locations/location-list/model/location-list-store";
import { EnvelopeError } from "@/shared/api/errors";
import { useCursorRef } from "@/shared/hooks/use-cursor-ref";
import { useInfiniteQuery } from "@tanstack/react-query";
import { useDebounce } from "use-debounce";

type Props = {
	stateId?: LocationListId;
	request?: GetLocationsInfinityRequest;
};

export function useInfiniteLocationsList({ stateId, request }: Props) {
	const selectedDepartments = useLocationSelectedDepartments(stateId);
	const search = useLocationSearch(stateId);
	const [debouncedSearch] = useDebounce(search, 600);
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
	} = useInfiniteQuery({
		...locationsApi.getLocationsInfinityQueryOptions({
			departmentIds: selectedDepartments.map((department) => department.id),
			search: debouncedSearch,
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
		isPending,
		isError,
		error: error instanceof EnvelopeError ? error : undefined,
		isFetchingNextPage,
		cursorRef,
		hasNextPage,
	};
}
