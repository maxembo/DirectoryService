import { apiClient } from "@/shared/api/axios-instance";
import {
	Envelope,
	envelopeInfinityQueryOptions,
	PaginationEnvelope,
} from "@/shared/api/envelops";
import { infiniteQueryOptions } from "@tanstack/react-query";
import {
	DepartmentId,
	DepartmentShortDto,
	DepartmentTreeDto,
} from "../model/types";
import {
	GetDepartmentChildrenRequest,
	GetDepartmentsInfinityRequest,
	GetDepartmentsRequest,
	GetDepartmentTreeRootsRequest,
	type MoveDepartmentRequest,
} from "./types";

const DEPARTMENTS_KEY = "departments";
const DEPARTMENTS_ENDPOINT = "/departments";

export const departmentsApi = {
	baseKey: DEPARTMENTS_KEY,

	getDepartmentsShort: async (request: GetDepartmentsRequest) => {
		const response = await apiClient.get<
			Envelope<PaginationEnvelope<DepartmentShortDto>>
		>(DEPARTMENTS_ENDPOINT, { params: request });

		return response.data;
	},

	getDepartmentsTreeRoots: async (request: GetDepartmentTreeRootsRequest) => {
		const response = await apiClient.get<
			Envelope<PaginationEnvelope<DepartmentTreeDto>>
		>(`${DEPARTMENTS_ENDPOINT}/tree`, { params: request });

		return response.data;
	},

	getDepartmentChildren: async ({
		parentId,
		...params
	}: GetDepartmentChildrenRequest) => {
		const response = await apiClient.get<
			Envelope<PaginationEnvelope<DepartmentTreeDto>>
		>(`${DEPARTMENTS_ENDPOINT}/${parentId}/children`, {
			params,
		});

		return response.data;
	},

	getDepartmentsInfinityQueryOptions: (
		request: GetDepartmentsInfinityRequest,
	) => {
		return infiniteQueryOptions({
			queryKey: [departmentsApi.baseKey, "list", request],
			queryFn: ({ pageParam }) => {
				return departmentsApi.getDepartmentsShort({
					...request,
					page: pageParam,
				});
			},
			...envelopeInfinityQueryOptions<DepartmentShortDto>(),
		});
	},

	getDepartmentTreeRootsInfinityQueryOptions: (
		request: GetDepartmentTreeRootsRequest,
	) => {
		return infiniteQueryOptions({
			queryKey: [departmentsApi.baseKey, "tree-roots", request],
			queryFn: ({ pageParam }) => {
				return departmentsApi.getDepartmentsTreeRoots({
					...request,
					page: pageParam,
				});
			},
			...envelopeInfinityQueryOptions<DepartmentTreeDto>(),
		});
	},

	getDepartmentChildrenInfinityOptions: (
		request: GetDepartmentChildrenRequest,
	) => {
		return infiniteQueryOptions({
			queryKey: [departmentsApi.baseKey, "children", request],
			queryFn: ({ pageParam }) => {
				return departmentsApi.getDepartmentChildren({
					...request,
					page: pageParam,
				});
			},
			...envelopeInfinityQueryOptions<DepartmentTreeDto>(),
		});
	},

	restoreDepartment: async (departmentId: DepartmentId) => {
		const response = await apiClient.patch<Envelope<DepartmentId>>(
			`${DEPARTMENTS_ENDPOINT}/${departmentId}/restore`,
		);

		return response.data;
	},

	moveDepartment: async (request: MoveDepartmentRequest) => {
		const response = await apiClient.put<Envelope<DepartmentId>>(
			`${DEPARTMENTS_ENDPOINT}/${request.departmentId}/parent`,
			{ parentId: request.parentId },
		);

		return response.data;
	},
};
