import { locationsApi } from "@/entities/locations/api/locations-api";
import { GetLocationsRequest } from "@/entities/locations/api/types";
import { EnvelopeError } from "@/shared/api/errors";
import { useCursorRef } from "@/shared/hooks/use-cursor-ref";
import { useInfiniteQuery as useInfinityQuery } from "@tanstack/react-query";

export function useInfinityLocationsList(params: GetLocationsRequest) {
	const {
		data,
		isPending,
		isError,
		error,
		hasNextPage,
		isFetchingNextPage,
		fetchNextPage,
	} = useInfinityQuery(locationsApi.getLocationsInfinityOptions(params));

	const cursorRef = useCursorRef({
		hasNextPage,
		isFetchingNextPage,
		fetchNextPage,
	});

	return {
		locations: data?.items ?? [],
		totalPages: data?.totalPages ?? 0,
		isPending: isPending,
		isError: isError,
		error: error instanceof EnvelopeError ? error : undefined,
		isFetchingNextPage,
		cursorRef: cursorRef,
	};
}
