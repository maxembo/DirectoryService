import { Card, CardContent } from "@/shared/components/ui/card";

export function LocationsListEmpty() {
	return (
		<Card className="border-dashed">
			<CardContent className="flex flex-col items-center justify-center py-10 text-center">
				<span className="text-lg font-semibold">Локации не найдены</span>
				<p className="mt-2 text-sm text-muted-foreground">
					Попробуй изменить параметры поиска.
				</p>
			</CardContent>
		</Card>
	);
}
