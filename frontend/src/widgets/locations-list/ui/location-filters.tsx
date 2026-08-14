import type { LocationSortByFilter } from "@/entities/locations";
import { FilterSelect } from "@/shared/components/filter-select";
import { SearchInput } from "@/shared/components/search-input";
import { sortDirectionOptions } from "@/shared/model/filter-options";
import { FilterOption } from "@/shared/model/filter-types";
import {
	setLocationSearch,
	setLocationSortBy,
	setLocationSortDirection,
	useLocationList,
	type LocationListId,
} from "../model/location-list-store";

const locationSortByOptions: Array<FilterOption<LocationSortByFilter>> = [
	{ value: "name", label: "По имени" },
	{ value: "created", label: "По дате создания" },
];

export function LocationFilters({ stateId }: { stateId?: LocationListId }) {
	const { search, sortBy, sortDirection } = useLocationList(stateId);

	return (
		<div className="space-y-4">
			<div className="flex items-center gap-4">
				<SearchInput
					className="w-full"
					value={search}
					placeholder="Поиск по названию"
					onChange={(value) => setLocationSearch(value, stateId)}
				/>

				<FilterSelect
					value={sortBy ?? "name"}
					onValueChange={(value) => setLocationSortBy(value, stateId)}
					items={locationSortByOptions}
				/>

				<FilterSelect
					value={sortDirection ?? "asc"}
					onValueChange={(value) => setLocationSortDirection(value, stateId)}
					items={sortDirectionOptions}
				/>
			</div>
		</div>
	);
}
