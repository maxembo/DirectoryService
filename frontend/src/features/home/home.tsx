import { routes } from "@/shared/routes";
import Image from "next/image";

const features = [
	{
		title: routes.departments.title,
		text: "Иерархическая структура отделов, филиалов и внутренних команд с поддержкой дерева и связей между узлами.",
	},
	{
		title: routes.locations.title,
		text: "Хранение офисов, зданий и других точек присутствия компании с привязкой к нескольким подразделениям.",
	},
	{
		title: routes.positions.title,
		text: "Каталог должностей, который можно использовать в разных отделах без дублирования данных.",
	},
];

const stats = [
	{ value: "1", label: "единый источник правды" },
	{ value: "3", label: "основных справочника" },
	{ value: "∞", label: "связей между сущностями" },
];

export default function Home() {
	return (
		<main className="min-h-screen bg-[radial-gradient(circle_at_top,rgba(59,130,246,0.12),transparent_35%),linear-gradient(to_bottom_right,#f8fafc,#eef2ff_50%,#ffffff)] text-slate-900">
			<section className="mx-auto w-full max-w-7xl px-6 py-16">
				<div className="grid items-center gap-12 lg:grid-cols-2">
					<div>
						<div className="mb-6 inline-flex items-center rounded-full border border-blue-200 bg-blue-50 px-4 py-2 text-sm font-medium text-blue-700 shadow-sm">
							Directory Service
						</div>

						<h1 className="max-w-2xl text-5xl font-bold tracking-tight text-slate-900 sm:text-6xl">
							Единый каталог оргструктуры компании
						</h1>

						<p className="mt-6 max-w-2xl text-lg leading-8 text-slate-600">
							Directory Service хранит базовые справочники оргструктуры — подразделения,
							должности и локации — и предоставляет единый CRUD-интерфейс для всех
							внутренних сервисов: HR, Склад, Заказы, Скидки и других.
						</p>

						<p className="mt-4 max-w-2xl text-lg leading-8 text-slate-600">
							Благодаря этому данные не дублируются, названия не расходятся между
							системами, а компания получает консистентную и управляемую модель
							организационной структуры.
						</p>

						<div className="mt-8 flex flex-wrap gap-3">
							{stats.map((item) => (
								<div
									key={item.label}
									className="rounded-2xl border border-slate-200 bg-white px-5 py-4 shadow-sm"
								>
									<div className="text-2xl font-bold text-slate-900">{item.value}</div>
									<div className="text-sm text-slate-500">{item.label}</div>
								</div>
							))}
						</div>
					</div>

					<div className="relative">
						<div className="absolute -inset-4 -z-10 rounded-[2rem] bg-blue-200/30 blur-3xl" />
						<div className="overflow-hidden rounded-[2rem] border border-slate-200 bg-white shadow-2xl">
							<div className="border-b border-slate-200 bg-slate-50 px-6 py-4">
								<p className="text-sm font-medium uppercase tracking-[0.2em] text-slate-500">
									Схема оргструктуры
								</p>
							</div>
							<div className="p-6">
								<Image
									src="/images/org-structure.png"
									alt="Схема организационной структуры компании"
									width={1200}
									height={700}
									className="h-auto w-full rounded-2xl object-contain"
									priority
								/>
							</div>
						</div>
					</div>
				</div>

				<div className="mt-16">
					<div className="mb-8">
						<h2 className="text-3xl font-bold tracking-tight text-slate-900">
							Что хранит Directory Service
						</h2>
						<p className="mt-3 max-w-3xl text-slate-600">
							Сервис нужен для того, чтобы все внутренние системы работали с одной и той же
							моделью данных, а не создавали свои копии справочников.
						</p>
					</div>

					<div className="grid gap-6 md:grid-cols-3">
						{features.map((feature) => (
							<article
								key={feature.title}
								className="group rounded-[1.75rem] border border-slate-200 bg-white p-6 shadow-sm transition-all duration-300 hover:-translate-y-1 hover:shadow-xl"
							>
								<div className="mb-4  mx-auto flex h-12 max-w-40 w-full items-center justify-center rounded-2xl bg-linear-to-br from-blue-500 to-indigo-600 text-white shadow-lg shadow-blue-500/20">
									<span className="text-lg font-semibold">{feature.title}</span>
								</div>
								<p className="mt-3 text-center leading-7 text-slate-600">{feature.text}</p>
							</article>
						))}
					</div>
				</div>

				<div className="mt-16 grid gap-6 lg:grid-cols-2">
					<div className="rounded-[1.75rem] border border-slate-200 bg-white p-8 shadow-sm">
						<h2 className="text-2xl font-bold text-slate-900">Зачем это нужно</h2>
						<ul className="mt-5 space-y-4 text-slate-600">
							<li>— Видеть структуру компании: кто кому   подчиняется и где работает.</li>
							<li>— Быстро находить отделы, должности и офисы в единой системе.</li>
							<li>— Хранить историю изменений и использовать данные для отчётов.</li>
							<li>— Исключить расхождения между сервисами и убрать дублирование справочников.</li>
						</ul>
					</div>

					<div className="rounded-[1.75rem] bg-linear-to-br from-slate-900 via-slate-800 to-slate-900 p-8 text-white shadow-2xl">
						<h2 className="text-2xl font-bold">Итог</h2>
						<p className="mt-5 leading-8 text-slate-300">
							Это не просто справочник, а фундаментальная часть платформы. Directory Service
							становится центром управления организационной моделью компании и обеспечивает
							единообразие данных для всех сервисов экосистемы.
						</p>
						<div className="mt-8 rounded-2xl border border-white/10 bg-white/5 p-5 backdrop-blur">
							<p className="text-sm uppercase tracking-[0.18em] text-slate-400">
								Ключевая идея
							</p>
							<p className="mt-2 text-lg font-medium">
								Один сервис — одна актуальная структура компании.
							</p>
						</div>
					</div>
				</div>
			</section>
		</main>
	);
}