import { Button } from "@/shared/components/ui/button";
import {
	Dialog,
	DialogClose,
	DialogContent,
	DialogFooter,
	DialogHeader,
	DialogTitle,
	DialogTrigger,
} from "@/shared/components/ui/dialog";
import { RotateCcw } from "lucide-react";
import { useState } from "react";
import { useRestoreLocation } from "../model/use-restore-location";
import type { LocationId } from "@/entities/locations";

type Props = {
	locationId: LocationId;
	name: string;
};

export function RestoreLocationDialog({ locationId, name }: Props) {
	const { restoreLocation, isPending } = useRestoreLocation();
	const [open, setOpen] = useState(false);

	const handleRestore = async (event: React.MouseEvent<HTMLButtonElement>) => {
		event.preventDefault();
		event.stopPropagation();

		try {
			await restoreLocation(locationId);
			setOpen(false);
		} catch {}
	};

	const handleClose = () => {
		setOpen(false);
	};

	const handleOpenChange = (nextOpen: boolean) => {
		if (!nextOpen) {
			handleClose();
			return;
		}

		setOpen(true);
	};

	return (
		<Dialog open={open} onOpenChange={handleOpenChange}>
			<DialogTrigger asChild>
				<Button
					type="button"
					variant="link"
					size="icon"
					className="h-8 w-8 hover:bg-blue-500"
					disabled={isPending}
					title="Восстановить"
				>
					<RotateCcw className="h-4 w-4" />
				</Button>
			</DialogTrigger>

			<DialogContent>
				<DialogHeader>
					<DialogTitle>Восстановить локацию</DialogTitle>
				</DialogHeader>
				<div className="min-w-0">
					<p>Вы уверены, что хотите восстановить локацию?</p>

					<p className="mt-2 max-h-32 max-w-full overflow-y-auto text-xl font-medium wrap-break-word text-blue-500">
						«{name}»
					</p>
				</div>

				<DialogFooter className="pt-2">
					<DialogClose asChild>
						<Button type="button" variant="outline" onClick={handleClose}>
							Отмена
						</Button>
					</DialogClose>
					<Button type="button" disabled={isPending} onClick={handleRestore}>
						{isPending ? "Восстановление..." : "Восстановить"}
					</Button>
				</DialogFooter>
			</DialogContent>
		</Dialog>
	);
}
