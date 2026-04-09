import { locationsApi } from "@/entities/locations/api/locations-api";
import { GetLocationsRequest } from "@/entities/locations/api/types";
import { EnvelopeError } from "@/shared/api/errors";
import { useQuery } from "@tanstack/react-query";

export function useLocationsQuery(params: GetLocationsRequest) {
	const query = useQuery(locationsApi.getLocationsQueryOptions(params));

	return {
		locations: query.data?.result?.items ?? [],
		totalPages: query.data?.result?.totalPages ?? 0,
		isPending: query.isPending,
		isError: query.isError,
		error: query.error instanceof EnvelopeError ? query.error : undefined,
	};
}
