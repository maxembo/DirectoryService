import { SortDirectionFilter } from "@/entities/locations/api/types";
import { InfinityScrollRequest } from "@/shared/api/infinity-scroll-request";
import { PaginationRequest } from "@/shared/api/pagination-request";

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
