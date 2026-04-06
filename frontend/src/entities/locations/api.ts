import { apiClient } from "@/shared/api/axios-instance";
import { Envelope, PaginationEnvelope } from "@/shared/api/envelops";
import { SortByFilter, SortDirectionFilter } from "@/shared/api/filters";
import { PaginationRequest } from "@/shared/api/pagination-request";
import { queryOptions } from "@tanstack/react-query";
import { AddressDto, Location } from "./types";

const LOCATIONS_KEY = "locations";
const LOCATIONS_ENDPOINT = "/locations";

export interface GetLocationsRequest extends PaginationRequest {
	departmentIds?: string[];
	search?: string;
	isActive?: boolean;
	sortBy?: SortByFilter;
	sortDirection?: SortDirectionFilter;
}

export type CreateLocationRequest = {
	name: string;
	address: AddressDto;
	timezone: string;
};

export const locationsApi = {
	baseKey: LOCATIONS_KEY,

	getLocationsQueryOptions: (request: GetLocationsRequest) =>
		queryOptions({
			queryKey: [locationsApi.baseKey, request],
			queryFn: async ({ signal }) => {
				const response = await apiClient.get<
					Envelope<PaginationEnvelope<Location>>
				>(LOCATIONS_ENDPOINT, {
					params: request,
					signal,
				});
				return response.data;
			},
		}),

	createLocation: async (request: CreateLocationRequest) => {
		const response = await apiClient.post<Envelope<Location>>(
			LOCATIONS_ENDPOINT,
			request,
		);
		return response.data;
	},
};
