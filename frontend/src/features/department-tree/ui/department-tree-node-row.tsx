import type { DepartmentTreeDto } from "@/entities/departments";
import { cn } from "@/shared/lib/utils";
import { ChevronRight, LoaderCircle } from "lucide-react";
import {
	TREE_BASE_PADDING,
	TREE_GUIDE_OFFSET,
	TREE_INDENT,
} from "./department-tree.constants";

type Props = {
	department: DepartmentTreeDto;
	depth: number;
	isLoading: boolean;
	isSelected: boolean;
	isExpanded: boolean;
	hasChildren: boolean;
	onToggle: () => void;
	onSelect: () => void;
};

export function DepartmentTreeNodeRow({
	department,
	depth,
	isSelected,
	isLoading,
	isExpanded,
	hasChildren,
	onToggle,
	onSelect,
}: Props) {
	return (
		<div
			className={cn(
				"group relative flex min-h-10 items-center gap-1 rounded-md pr-2 transition-colors",
				isSelected
					? "bg-accent text-accent-foreground ring-border ring-1 ring-inset"
					: "hover:bg-muted/70",
			)}
			style={{
				paddingLeft: TREE_BASE_PADDING + depth * TREE_INDENT,
			}}
		>
			<TreeGuideLines depth={depth} />

			<div className="relative z-10 flex size-6 shrink-0 items-center justify-center">
				{hasChildren ? (
					<button
						type="button"
						className={cn(
							"text-muted-foreground flex size-6 items-center justify-center rounded-sm",
							"hover:bg-background hover:text-foreground",
							"focus-visible:ring-ring focus-visible:ring-2 focus-visible:outline-none",
						)}
						aria-expanded={isExpanded}
						title={
							isExpanded
								? `Свернуть подразделение ${department.name}`
								: `Развернуть подразделение ${department.name}`
						}
						aria-label={
							isExpanded
								? `Свернуть подразделение ${department.name}`
								: `Развернуть подразделение ${department.name}`
						}
						onClick={onToggle}
					>
						{isLoading ? (
							<LoaderCircle className="size-4 animate-spin" />
						) : (
							<ChevronRight
								className={cn(
									"size-4 transition-transform duration-150",
									isExpanded && "rotate-90",
								)}
							/>
						)}
					</button>
				) : (
					<span
						className="bg-muted-foreground/40 size-1.5 rounded-full"
						aria-hidden="true"
					/>
				)}
			</div>

			<button
				type="button"
				className={cn(
					"relative z-10 flex min-w-0 flex-1 items-center gap-3 py-2 text-left",
					"focus-visible:outline-none",
				)}
				onClick={onSelect}
			>
				<span className="min-w-0 flex-1">
					<span
						className={cn(
							"block truncate text-sm",
							isSelected ? "font-semibold" : "font-medium",
						)}
					>
						{department.name}
					</span>
				</span>

				<span
					className={cn(
						"max-w-32 shrink-0 truncate rounded px-1.5 py-0.5",
						"font-mono text-[10px] leading-none",
						isSelected
							? "bg-background/70 text-foreground"
							: "bg-muted text-muted-foreground",
					)}
					title={department.identifier}
				>
					{department.identifier}
				</span>
			</button>
		</div>
	);
}

type TreeGuideLinesProps = {
	depth: number;
};

function TreeGuideLines({ depth }: TreeGuideLinesProps) {
	if (depth === 0) return null;

	return (
		<>
			{Array.from({ length: depth }).map((_, index) => (
				<span
					key={index}
					className="pointer-events-none absolute inset-y-0 w-px bg-blue-400"
					style={{
						left: TREE_GUIDE_OFFSET + index * TREE_INDENT,
					}}
					aria-hidden="true"
				/>
			))}
		</>
	);
}
