import {
	SortByFilter,
	SortDirectionFilter,
} from "@/entities/locations/api/types";
import { setFilterIsActive, setFilterSearch, setFilterSortBy, setFilterSortDirection, useGetLocationFilters } from "@/features/locations/model/locations-filter-store";
import { Input } from "@/shared/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/shared/components/ui/select";
import { ActiveFilter } from "@/widgets/model/types";
import { Search } from "lucide-react";

type FilterSelectProps<T extends string> = {
	value: T;
	onValueChange: (value: T) => void;
	placeholder: string;
	items: Array<{
		value: T;
		label: string;
	}>;
};

function FilterSelect<T extends string>({ value, onValueChange, placeholder, items }: FilterSelectProps<T>) {
	return (
		<Select value={value} onValueChange={(value) => onValueChange(value as T)}>
			<SelectTrigger>
				<SelectValue placeholder={placeholder} />
			</SelectTrigger>
			<SelectContent position="popper" side="bottom" sideOffset={4}>
				{items.map((item) => (
					<SelectItem key={item.value} value={item.value}>{item.label}</SelectItem>
				))}
			</SelectContent>

		</Select>
	)
}

const activeItems: Array<{ value: ActiveFilter, label: string }> = [
	{ value: "all", label: "Все" },
	{ value: "active", label: "Активные" },
	{ value: "inactive", label: "Неактивные" },
];

const sortByItems: Array<{ value: SortByFilter, label: string }> = [
	{ value: "name", label: "По имени" },
	{ value: "created", label: "По дате создания" },
];

const sortDirectionItems: Array<{ value: SortDirectionFilter, label: string }> = [
	{ value: "asc", label: "По возрастанию" },
	{ value: "desc", label: "По убыванию" },
];

export function LocationFilters() {

	const { search, isActive, sortBy, sortDirection } = useGetLocationFilters();

	return (
		<div className="space-y-4">
			<div className="flex items-center gap-4">
				<div className="flex-1 relative">
					<Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
					<Input
						placeholder="Поиск по названию"
						className="pl-9"
						value={search ?? ""}
						onChange={(e) => setFilterSearch(e.target.value)}
					/>
				</div>
				<FilterSelect
					value={isActive ?? "all"}
					onValueChange={setFilterIsActive}
					items={activeItems}
					placeholder="Статус"
				/>

				<FilterSelect
					value={sortBy ?? "name"}
					onValueChange={setFilterSortBy}
					items={sortByItems}
					placeholder="Сортировка"
				/>

				<FilterSelect
					value={sortDirection ?? "asc"}
					onValueChange={setFilterSortDirection}
					items={sortDirectionItems}
					placeholder="Направление"
				/>
			</div>
		</div>
	);
}
