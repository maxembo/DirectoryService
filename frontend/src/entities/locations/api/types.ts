import { InfinityScrollRequest } from "@/shared/api/infinity-scroll-request";
import { PaginationRequest } from "@/shared/api/pagination-request";
import { AddressDto } from "../model/types";

export type SortByFilter = "name" | "created";
export type SortDirectionFilter = "asc" | "desc";

export interface GetLocationsRequest extends PaginationRequest {
	departmentIds?: string[];
	search?: string;
	isActive?: boolean;
	sortBy?: SortByFilter;
	sortDirection?: SortDirectionFilter;
}

export interface GetLocationsInfinityRequest extends InfinityScrollRequest {
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

export type UpdateLocationRequest = {
	name: string;
	address: AddressDto;
	timezone: string;
};
