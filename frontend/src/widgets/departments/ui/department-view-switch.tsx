import { Button } from "@/shared/components/ui/button";
import type { ArchiveView } from "@/shared/hooks";

type Props = {
	value: ArchiveView;
	onValueChange: (value: ArchiveView) => void;
};

export function DepartmentViewSwitch({ value, onValueChange }: Props) {
	return (
		<div
			role="group"
			className="bg-muted flex justify-end gap-2 rounded-2xl border p-2"
			aria-label="Режим отображения подразделений"
		>
			<Button
				type="button"
				aria-pressed={value === "active"}
				variant={value === "active" ? "default" : "outline"}
				onClick={() => onValueChange("active")}
			>
				Структура
			</Button>
			<Button
				type="button"
				aria-pressed={value === "archived"}
				variant={value === "archived" ? "default" : "outline"}
				onClick={() => onValueChange("archived")}
			>
				Удалённые
			</Button>
		</div>
	);
}
