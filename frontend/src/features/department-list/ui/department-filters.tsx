import { SearchInput } from "@/shared/components/search-input";

type Props = {
	search: string;
	setSearch: (value: string) => void;
};
export function DepartmentFilters({ search, setSearch }: Props) {
	return (
		<div className="space-y-4">
			<div className="flex items-center gap-4">
				<SearchInput
					className="w-full"
					value={search}
					placeholder="Поиск по названию"
					onChange={(value) => setSearch(value)}
				/>
			</div>
		</div>
	);
}
