import { InfinityScrollRequest } from "@/shared/api/infinity-scroll-request";
import { PaginationRequest } from "@/shared/api/pagination-request";
import { SortDirectionFilter } from "@/shared/model/filter-types";

export interface GetPositionsRequest extends PaginationRequest {
	departmentIds?: string[];
	search?: string;
	isActive?: boolean;
	sortBy?: string;
	sortDirection?: SortDirectionFilter;
}

export interface GetPositionsInfinityRequest extends InfinityScrollRequest {
	departmentIds?: string[];
	search?: string;
	isActive?: boolean;
	sortBy?: string;
	sortDirection?: SortDirectionFilter;
}

export interface CreatePositionRequest {
	name: string;
	description?: string;
}
