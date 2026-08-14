import { Spinner } from "@/shared/components/ui/spinner";
import { InfiniteLocationsList } from "@/widgets/locations-list";

import { Suspense } from "react";

export default function LocationsPage() {
	return (
		<Suspense
			fallback={
				<div className="flex min-h-60 items-center justify-center">
					<Spinner />
				</div>
			}
		>
			<InfiniteLocationsList />
		</Suspense>
	);
}
