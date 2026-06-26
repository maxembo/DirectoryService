import {
	LocatinSortByFilter,
	SortDirectionFilter,
} from "@/entities/locations/api/types";
import { Input } from "@/shared/components/ui/input";
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from "@/shared/components/ui/select";
import { ActiveFilter } from "@/widgets/model/types";
import { Search } from "lucide-react";

type Props = {
	filters: {
		search: string;
		isActive: ActiveFilter;
		sortBy: LocatinSortByFilter;
		sortDirection: SortDirectionFilter;
	};
	actions: {
		setSearch: (search: string) => void;
		setIsActive: (isActive: ActiveFilter) => void;
		setSortBy: (sortBy: LocatinSortByFilter) => void;
		setSortDirection: (sortDirection: SortDirectionFilter) => void;
	};
};

type FilterSelectProps<T extends string> = {
	value: T;
	onValueChange: (value: T) => void;
	placeholder: string;
	items: Array<{
		value: T;
		label: string;
	}>;
};

function FilterSelect<T extends string>({
	value,
	onValueChange,
	placeholder,
	items,
}: FilterSelectProps<T>) {
	return (
		<Select value={value} onValueChange={(value) => onValueChange(value as T)}>
			<SelectTrigger>
				<SelectValue placeholder={placeholder} />
			</SelectTrigger>
			<SelectContent position="popper" side="bottom" sideOffset={4}>
				{items.map((item) => (
					<SelectItem key={item.value} value={item.value}>
						{item.label}
					</SelectItem>
				))}
			</SelectContent>
		</Select>
	);
}

const activeItems: Array<{ value: ActiveFilter; label: string }> = [
	{ value: "all", label: "Все" },
	{ value: "active", label: "Активные" },
	{ value: "inactive", label: "Неактивные" },
];

const sortByItems: Array<{ value: LocatinSortByFilter; label: string }> = [
	{ value: "name", label: "По имени" },
	{ value: "created", label: "По дате создания" },
];

const sortDirectionItems: Array<{ value: SortDirectionFilter; label: string }> =
	[
		{ value: "asc", label: "По возрастанию" },
		{ value: "desc", label: "По убыванию" },
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
					items={activeItems}
					placeholder="Статус"
				/>

				<FilterSelect
					value={sortBy}
					onValueChange={setSortBy}
					items={sortByItems}
					placeholder="Сортировка"
				/>

				<FilterSelect
					value={sortDirection}
					onValueChange={setSortDirection}
					items={sortDirectionItems}
					placeholder="Направление"
				/>
			</div>
		</div>
	);
}
