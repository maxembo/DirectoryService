import { FilterSelect } from "@/shared/components/filter-select";
import { SearchInput } from "@/shared/components/search-input";
import {
	activeFilterOptions,
	sortDirectionOptions,
} from "@/shared/model/filter-options";
import { FilterOption } from "@/shared/model/filter-types";
import {
	PositionListId,
	setPositionIsActive,
	setPositionSearch,
	setPositionSortBy,
	setPositionSortDirection,
	usePositionIsActive,
	usePositionSearch,
	usePositionSortBy,
	usePositionSortDirection,
} from "../model/position-list-store";
import type { PositionSortByFilter } from "@/entities/positions";

const positionSortByOptions: Array<FilterOption<PositionSortByFilter>> = [
	{ value: "name", label: "По имени" },
	{ value: "created", label: "По дате создания" },
	{ value: "updated", label: "По дате обновления" },
	{ value: "status", label: "По статусу" },
	{ value: "department_count", label: "По количеству отделов" },
];

export function PositionFilters({ stateId }: { stateId?: PositionListId }) {
	const search = usePositionSearch(stateId);
	const isActive = usePositionIsActive(stateId);
	const sortBy = usePositionSortBy(stateId);
	const sortDirection = usePositionSortDirection(stateId);

	return (
		<div className="shrink-0">
			<div className="flex min-w-0 items-center gap-2">
				<SearchInput
					className="min-w-0 flex-1"
					value={search}
					placeholder="Поиск позиции"
					onChange={(value) => setPositionSearch(value, stateId)}
				/>

				<FilterSelect
					value={isActive}
					onValueChange={(value) => setPositionIsActive(value, stateId)}
					items={activeFilterOptions}
				/>

				<FilterSelect
					value={sortBy}
					onValueChange={(value) => setPositionSortBy(value, stateId)}
					items={positionSortByOptions}
				/>

				<FilterSelect
					value={sortDirection}
					onValueChange={(value) => setPositionSortDirection(value, stateId)}
					items={sortDirectionOptions}
				/>
			</div>
		</div>
	);
}
