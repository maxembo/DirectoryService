import { LocationSortByFilter } from "@/entities/locations/model/types";
import { FilterSelect } from "@/shared/components/filter-select";
import { Input } from "@/shared/components/ui/input";
import {
	activeFilterOptions,
	sortDirectionOptions,
} from "@/shared/model/filter-options";
import {
	ActiveFilter,
	FilterOption,
	SortDirectionFilter,
} from "@/shared/model/filter-types";
import { Search } from "lucide-react";

type Props = {
	filters: {
		search: string;
		isActive: ActiveFilter;
		sortBy: LocationSortByFilter;
		sortDirection: SortDirectionFilter;
	};
	actions: {
		setSearch: (search: string) => void;
		setIsActive: (isActive: ActiveFilter) => void;
		setSortBy: (sortBy: LocationSortByFilter) => void;
		setSortDirection: (sortDirection: SortDirectionFilter) => void;
	};
};

const locationSortByOptions: Array<FilterOption<LocationSortByFilter>> = [
	{ value: "name", label: "По имени" },
	{ value: "created", label: "По дате создания" },
];

export function LocationFiltersPanel({ filters, actions }: Props) {
	const { search, isActive, sortBy, sortDirection } = filters;
	const { setSearch, setIsActive, setSortBy, setSortDirection } = actions;

	return (
		<div className="space-y-4">
			<div className="flex-1 relative">
				<Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
				<Input
					placeholder="Поиск по названию"
					className="pl-9"
					value={search}
					onChange={(e) => setSearch(e.target.value)}
				/>
			</div>
			<div className="flex items-center gap-4">
				<FilterSelect
					value={isActive}
					onValueChange={setIsActive}
					items={activeFilterOptions}
					placeholder="Статус"
				/>

				<FilterSelect
					value={sortBy}
					onValueChange={setSortBy}
					items={locationSortByOptions}
					placeholder="Сортировка"
				/>

				<FilterSelect
					value={sortDirection}
					onValueChange={setSortDirection}
					items={sortDirectionOptions}
					placeholder="Направление"
				/>
			</div>
		</div>
	);
}
