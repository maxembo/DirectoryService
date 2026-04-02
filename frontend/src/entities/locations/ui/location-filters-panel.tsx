import { Input } from "@/shared/components/ui/input";
import { NativeSelect } from "@/shared/components/ui/native-select";
import { ActiveFilter, SortByFilter, SortDirectionFilter } from "../types";

type Props = {
	search: string;
	setSearch: (search: string) => void;
	isActive: ActiveFilter;
	setIsActive: (isActive: ActiveFilter) => void;
	sortBy: SortByFilter;
	setSortBy: (sortBy: SortByFilter) => void;
	sortDirection: SortDirectionFilter;
	setSortDirection: (sortDirection: SortDirectionFilter) => void;
};

export function LocationFiltersPanel({
	search,
	setSearch,
	isActive,
	setIsActive,
	sortBy,
	setSortBy,
	sortDirection,
	setSortDirection,
}: Props) {
	return (
		<div className="space-y-4">
			<Input
				placeholder="Поиск"
				value={search}
				onChange={(e) => setSearch(e.target.value)}
			/>
			<div className="flex items-center gap-4">
				<NativeSelect
					value={isActive}
					onChange={(e) => setIsActive(e.target.value as ActiveFilter)}
				>
					<option value="all">Все</option>
					<option value="active">Активные</option>
					<option value="inactive">Неактивные</option>
				</NativeSelect>

				<NativeSelect
					value={sortBy}
					onChange={(e) => setSortBy(e.target.value as SortByFilter)}
				>
					<option value="name">По имени</option>
					<option value="created">По дате создания</option>
				</NativeSelect>

				<NativeSelect
					value={sortDirection}
					onChange={(e) =>
						setSortDirection(e.target.value as SortDirectionFilter)
					}
				>
					<option value="asc">По возрастанию</option>
					<option value="desc">По убыванию</option>
				</NativeSelect>
			</div>
		</div>
	);
}
