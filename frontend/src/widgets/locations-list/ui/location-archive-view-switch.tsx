import { Button } from "@/shared/components/ui/button";
import type { ArchiveView } from "@/shared/hooks";

type Props = {
	value: ArchiveView;
	onValueChange: (value: ArchiveView) => void;
};

export function LocationArchiveViewSwitch({ value, onValueChange }: Props) {
	return (
		<div
			role="group"
			className="bg-muted flex justify-end gap-2 rounded-2xl border p-2"
			aria-label="Режим отображения локаций"
		>
			<Button
				type="button"
				aria-pressed={value === "active"}
				variant={value === "active" ? "default" : "outline"}
				onClick={() => onValueChange("active")}
			>
				Активные
			</Button>
			<Button
				type="button"
				aria-pressed={value === "archived"}
				variant={value === "archived" ? "default" : "outline"}
				onClick={() => onValueChange("archived")}
			>
				Архивные
			</Button>
		</div>
	);
}
