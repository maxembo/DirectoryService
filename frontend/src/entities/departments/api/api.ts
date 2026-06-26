import { apiClient } from "@/shared/api/axios-instance";
import {
	Envelope,
	envelopeInfinityQueryOptions,
	PaginationEnvelope,
} from "@/shared/api/envelops";
import { infiniteQueryOptions } from "@tanstack/react-query";
import { DepartmentShortDto } from "../model/types";
import { GetDepartmentsInfinityRequest, GetDepartmentsRequest } from "./types";

const DEPARTMENTS_KEY = "departments";
const DEPARTMENTS_ENDPOINT = "/departments";

export const departmentsApi = {
	baseKey: DEPARTMENTS_KEY,

	getDepartments: async (request: GetDepartmentsRequest) => {
		const response = await apiClient.get<
			Envelope<PaginationEnvelope<DepartmentShortDto>>
		>(DEPARTMENTS_ENDPOINT, { params: request });

		return response.data;
	},

	getDepartmentsInfinityQueryOptions: (
		request: GetDepartmentsInfinityRequest,
	) => {
		return infiniteQueryOptions({
			queryKey: [
				departmentsApi.baseKey,
				request.selectedLocations,
				request.search,
				request.isParent,
				request.parentId,
				request.isActive,
				request.sortBy,
				request.sortDirection,
				request.pageSize,
			],
			queryFn: ({ pageParam }) => {
				return departmentsApi.getDepartments({ ...request, page: pageParam });
			},
			...envelopeInfinityQueryOptions<DepartmentShortDto>(),
		});
	},
};
