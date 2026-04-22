import { locationsApi } from "@/entities/locations/api/locations-api";
import { GetLocationsRequest } from "@/entities/locations/api/types";
import { EnvelopeError } from "@/shared/api/errors";
import { useInfiniteQuery as useInfinityQuery } from "@tanstack/react-query";
import { RefCallback, useCallback } from "react";

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

	const cursorRef: RefCallback<HTMLDivElement> = useCallback(
		(el) => {
			const observer = new IntersectionObserver(
				(entries) => {
					if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) {
						fetchNextPage();
					}
				},
				{ threshold: 1 },
			);

			if (el) {
				observer.observe(el);

				return () => observer.disconnect();
			}
		},
		[hasNextPage, isFetchingNextPage, fetchNextPage],
	);

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
