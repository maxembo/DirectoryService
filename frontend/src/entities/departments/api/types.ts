import { InfinityScrollRequest } from "@/shared/api/infinity-scroll-request";
import { PaginationRequest } from "@/shared/api/pagination-request";
import { SortDirectionFilter } from "@/shared/model/filter-types";

export interface GetDepartmentsRequest extends PaginationRequest {
	selectedLocations?: string[];
	search?: string;
	isActive?: boolean;
	isParent?: boolean;
	parentId?: string;
	sortBy?: string;
	sortDirection?: SortDirectionFilter;
}

export interface GetDepartmentsInfinityRequest extends InfinityScrollRequest {
	selectedLocations?: string[];
	search?: string;
	isActive?: boolean;
	isParent?: boolean;
	parentId?: string;
	sortBy?: string;
	sortDirection?: SortDirectionFilter;
}

export interface GetDepartmentTreeRootsRequest extends PaginationRequest {
	prefetch?: number;
}

export interface GetDepartmentChildrenRequest extends PaginationRequest {
	parentId: string;
}
