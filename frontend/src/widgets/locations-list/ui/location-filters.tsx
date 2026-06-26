import {
	LocatinSortByFilter,
	SortDirectionFilter,
} from "@/entities/locations/api/types";
import {
	setLocationIsActive,
	setLocationSearch,
	setLocationSortBy,
	setLocationSortDirection,
	useLocationList,
} from "@/features/locations/model/location-list-store";
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

export type FilterSelectProps<T extends string> = {
	value: T;
	onValueChange: (value: T) => void;
	label?: string;
	items: Array<{
		value: T;
		label: string;
	}>;
};

export function FilterSelect<T extends string>({
	value,
	onValueChange,
	label,
	items,
}: FilterSelectProps<T>) {
	return (
		<div className="flex flex-col gap-2">
			<label className="text-sm font-medium">{label}</label>
			<Select
				value={value}
				onValueChange={(value) => onValueChange(value as T)}
			>
				<SelectTrigger>
					<SelectValue />
				</SelectTrigger>
				<SelectContent position="popper" side="bottom" sideOffset={4}>
					{items.map((item) => (
						<SelectItem key={item.value} value={item.value}>
							{item.label}
						</SelectItem>
					))}
				</SelectContent>
			</Select>
		</div>
	);
}

export const activeItems: Array<{ value: ActiveFilter; label: string }> = [
	{ value: "all", label: "Все" },
	{ value: "active", label: "Активные" },
	{ value: "inactive", label: "Неактивные" },
];

const sortByItems: Array<{ value: LocatinSortByFilter; label: string }> = [
	{ value: "name", label: "По имени" },
	{ value: "created", label: "По дате создания" },
];

export const sortDirectionItems: Array<{
	value: SortDirectionFilter;
	label: string;
}> = [
	{ value: "asc", label: "По возрастанию" },
	{ value: "desc", label: "По убыванию" },
];

export function LocationFilters() {
	const { search, isActive, sortBy, sortDirection } = useLocationList();

	return (
		<div className="space-y-4">
			<div className="flex items-center gap-4">
				<div className="flex-1 relative">
					<Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
					<Input
						placeholder="Поиск по названию"
						className="pl-9"
						value={search ?? ""}
						onChange={(e) => setLocationSearch(e.target.value)}
					/>
				</div>
				<FilterSelect
					value={isActive ?? "all"}
					onValueChange={setLocationIsActive}
					items={activeItems}
				/>

				<FilterSelect
					value={sortBy ?? "name"}
					onValueChange={setLocationSortBy}
					items={sortByItems}
				/>

				<FilterSelect
					value={sortDirection ?? "asc"}
					onValueChange={setLocationSortDirection}
					items={sortDirectionItems}
				/>
			</div>
		</div>
	);
}
