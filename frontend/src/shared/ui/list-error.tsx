import { Button } from "../components/ui/button";
import { Card, CardContent } from "../components/ui/card";

type Props = {
	message: string;
	onRetry?: () => void | Promise<unknown>;
};

export function ListError({ message, onRetry }: Props) {
	return (
		<Card className="border-destructive/40">
			<CardContent className="space-y-3 py-10 text-center">
				<p className="text-destructive text-sm font-medium">
					Ошибка: {message}
				</p>

				{onRetry && (
					<Button
						type="button"
						variant="outline"
						onClick={() => void onRetry()}
					>
						Повторить
					</Button>
				)}
			</CardContent>
		</Card>
	);
}
