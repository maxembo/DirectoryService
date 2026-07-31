import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from "@/shared/components/ui/select";
import { cn } from "@/shared/lib/utils";
import { FilterOption } from "@/shared/model/filter-types";

export type FilterSelectProps<T extends string> = {
	value: T;
	onValueChange: (value: T) => void;
	items: Array<FilterOption<T>>;
	label?: string;
	placeholder?: string;
	className?: string;
};

export function FilterSelect<T extends string>({
	value,
	onValueChange,
	items,
	label,
	placeholder,
	className,
}: FilterSelectProps<T>) {
	return (
		<div className={cn("flex flex-col gap-2", className)}>
			{label && <label className="text-sm font-medium">{label}</label>}
			<Select
				value={value}
				onValueChange={(value) => onValueChange(value as T)}
			>
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
		</div>
	);
}
