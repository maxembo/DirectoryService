import { Card, CardContent } from "@/shared/components/ui/card";

type Props = { message: string };

export function LocationsListError({ message }: Props) {
	return (
		<Card className="border-destructive/40">
			<CardContent className="py-10 text-center">
				<p className="text-sm font-medium text-destructive">
					Ошибка: {message}
				</p>
			</CardContent>
		</Card>
	);
}
