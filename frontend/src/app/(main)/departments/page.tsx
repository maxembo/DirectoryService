import { Spinner } from "@/shared/components/ui/spinner";
import { DepartmentView } from "@/widgets/departments";
import { Suspense } from "react";

export default function DepartmentsPage() {
	return (
		<Suspense fallback={<SuspenseFallback />}>
			<DepartmentView />
		</Suspense>
	);
}

const SuspenseFallback = () => (
	<div className="flex items-center justify-center">
		<Spinner />
	</div>
);
