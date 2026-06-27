import { InfinityScrollRequest } from "@/shared/api/infinity-scroll-request";
import { PaginationRequest } from "@/shared/api/pagination-request";
import { SortDirectionFilter } from "@/shared/model/filter-types";
import { AddressDto } from "../model/types";

export interface GetLocationsRequest extends PaginationRequest {
	departmentIds?: string[];
	search?: string;
	isActive?: boolean;
	sortBy?: string;
	sortDirection?: SortDirectionFilter;
}

export interface GetLocationsInfinityRequest extends InfinityScrollRequest {
	departmentIds?: string[];
	search?: string;
	isActive?: boolean;
	sortBy?: string;
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
