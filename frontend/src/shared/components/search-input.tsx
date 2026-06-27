import { Input } from "@/shared/components/ui/input";
import { Search } from "lucide-react";

type Props = {
	value: string;
	onChange: (value: string) => void;
	placeholder?: string;
	className?: string;
};

export function SearchInput({
	value,
	onChange,
	placeholder = "Поиск",
	className,
}: Props) {
	return (
		<div className={className}>
			<div className="relative">
				<Search className="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />

				<Input
					value={value}
					placeholder={placeholder}
					className="h-12 pl-11"
					onChange={(event) => onChange(event.target.value)}
				/>
			</div>
		</div>
	);
}
