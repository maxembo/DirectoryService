import { Button } from "@/shared/components/ui/button";
import { X } from "lucide-react";
import { Spinner } from "@/shared/components/ui/spinner";
import { ListError } from "@/shared/ui/list-error";
import { useFocusCardRef } from "@/shared/hooks";
import { cn } from "@/shared/lib/utils";

type Props = {
	isLoading: boolean;
	isError: boolean;
	onRemove: () => void;
	error?: Error | null;
	refetch?: () => void;
	children?: React.ReactNode;
	isSelected?: boolean;
};

export function FocusedCard({
	children,
	onRemove,
	isLoading,
	isError,
	error,
	refetch,
	isSelected = true,
}: Props) {
	const focusedRef = useFocusCardRef();

	return (
		<section className="space-y-3">
			<div className="flex gap-3">
				<h2 className="text-muted-foreground text-sm font-medium">
					Найдено через глобальный поиск
				</h2>
				<Button
					type="button"
					size="icon-xs"
					onClick={onRemove}
					aria-label="Убрать найденную карточку"
				>
					<X />
				</Button>
			</div>

			{isLoading ? (
				<div className="flex min-h-60 items-center justify-center">
					<Spinner />
				</div>
			) : isError ? (
				<ListError
					message={error?.message ?? "Неизвестная ошибка"}
					onRetry={refetch}
				/>
			) : (
				<div
					id="global-search-focused-card"
					className={cn(
						"w-full rounded-xl",
						isSelected && "ring-primary ring-2",
					)}
					tabIndex={-1}
					ref={focusedRef}
				>
					{children}
				</div>
			)}
		</section>
	);
}
