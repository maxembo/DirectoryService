import { Card, CardContent } from "@/shared/components/ui/card";

type Props = {
	title: string;
};

export function ListEmpty({ title }: Props) {
	return (
		<Card className="border-dashed">
			<CardContent className="flex flex-col items-center justify-center py-10 text-center">
				<span className="text-lg font-semibold">{title}</span>
				<p className="mt-2 text-sm text-muted-foreground">
					Попробуй изменить параметры поиска.
				</p>
			</CardContent>
		</Card>
	);
}
