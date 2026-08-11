import { Button } from "@/shared/components/ui/button";
import { ArchiveView } from "@/shared/hooks/use-archive-view";

type Props = {
	value: ArchiveView;
	onValueChange: (value: ArchiveView) => void;
	title?: string;
};

export function ArchiveViewSwitch({ value, onValueChange, title }: Props) {
	return (
		<div
			role="group"
			className="flex gap-2 justify-end rounded-2xl border bg-muted p-2"
			title={title ?? "Режим отображения"}
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
