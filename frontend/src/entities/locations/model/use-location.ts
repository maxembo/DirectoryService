import { useQuery } from "@tanstack/react-query";
import { locationsApi } from "../api/api";
import type { LocationId } from "./types";

export function useLocation(id: LocationId) {
	const query = useQuery({ ...locationsApi.getLocationQueryOptions(id) });

	return {
		location: query.data?.result,
		isLoading: query.isLoading,
		isError: query.isError,
		error: query.error,
		refetch: query.refetch,
	};
}
