import { apiClient } from "@/shared/api/axios-instance";
import { Envelope, PaginationEnvelope } from "@/shared/api/envelops";
import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";
import { Location } from "../model/types";
import {
	CreateLocationRequest,
	GetLocationsInfinityRequest,
	GetLocationsRequest,
	UpdateLocationRequest,
} from "./types";

const LOCATIONS_KEY = "locations";
const LOCATIONS_ENDPOINT = "/locations";

export const locationsApi = {
	baseKey: LOCATIONS_KEY,

	getLocations: async (request: GetLocationsRequest) => {
		const response = await apiClient.get<
			Envelope<PaginationEnvelope<Location>>
		>(LOCATIONS_ENDPOINT, { params: request });

		return response.data.result;
	},

	getLocationsQueryOptions: (request: GetLocationsRequest) =>
		queryOptions({
			queryKey: [locationsApi.baseKey, request],
			queryFn: () => {
				return locationsApi.getLocations(request);
			},
		}),

	getLocationsInfinityOptions: (request: GetLocationsInfinityRequest) => {
		return infiniteQueryOptions({
			queryKey: [
				locationsApi.baseKey,
				request.search,
				request.isActive,
				request.sortBy,
				request.sortDirection,
				request.pageSize,
			],
			queryFn: ({ pageParam }) => {
				return locationsApi.getLocations({ ...request, page: pageParam });
			},
			initialPageParam: 1,
			getNextPageParam: (response) => {
				if (!response || response.page >= response.totalPages) {
					return undefined;
				}

				return response.page + 1;
			},
			select: (data): PaginationEnvelope<Location> => {
				return {
					items: data.pages.flatMap((page) => page?.items ?? []),
					totalCount: data.pages[0]?.totalCount ?? 0,
					page: data.pages[data.pages.length - 1]?.page ?? 1,
					pageSize: data.pages[0]?.pageSize ?? 0,
					totalPages: data.pages[0]?.totalPages ?? 0,
				};
			},
		});
	},

	createLocation: async (request: CreateLocationRequest) => {
		const response = await apiClient.post<Envelope<Location>>(
			LOCATIONS_ENDPOINT,
			request,
		);

		return response.data;
	},

	updateLocation: async (
		request: UpdateLocationRequest & { locationId: string },
	) => {
		const response = await apiClient.patch<Envelope<Location>>(
			`${LOCATIONS_ENDPOINT}/${request.locationId}`,
			request,
		);

		return response.data;
	},

	deleteLocation: async (locationId: string) => {
		const response = await apiClient.delete<Envelope<Location>>(
			`${LOCATIONS_ENDPOINT}/${locationId}`,
		);

		return response.data;
	},
};
