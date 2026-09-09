"use client";

import { useState } from "react";
import {
  Button,
  Input,
  Textarea,
  Select,
  Checkbox,
  Radio,
  Modal,
  Drawer,
  Badge,
  Card,
  CardHeader,
  CardBody,
  CardFooter,
  Skeleton,
  Pagination,
  Tabs,
  Dropdown,
} from "@/components/ui";
import { Container } from "@/components/layout/Container";
import { useUiStore } from "@/lib/store/ui-store";

export default function HomePage() {
  const [modalOpen, setModalOpen] = useState(false);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [page, setPage] = useState(1);
  const pushToast = useUiStore((s) => s.pushToast);

  return (
    <Container className="flex flex-col gap-10 py-8">
      <section>
        <h1 className="text-[var(--text-display)] font-extrabold text-[var(--color-ink-900)]">
          پایه Design System
        </h1>
        <p className="mt-2 max-w-xl text-[var(--text-body)] text-[var(--color-ink-600)]">
          این صفحه صرفاً برای بررسی بصری Componentهای پایه ساخته شده و بخشی از تجربه نهایی
          محصول نیست. Featureهای واقعی مارکت‌پلیس در فازهای بعدی اضافه می‌شوند.
        </p>
      </section>

      <section className="flex flex-col gap-3">
        <h2 className="text-[var(--text-h2)] font-bold">دکمه‌ها</h2>
        <div className="flex flex-wrap items-center gap-3">
          <Button variant="primary">اصلی</Button>
          <Button variant="secondary">ثانویه</Button>
          <Button variant="outline">حاشیه‌دار</Button>
          <Button variant="ghost">شفاف</Button>
          <Button variant="danger">خطر</Button>
          <Button isLoading>در حال ارسال</Button>
          <Button disabled>غیرفعال</Button>
        </div>
      </section>

      <section className="flex flex-col gap-3">
        <h2 className="text-[var(--text-h2)] font-bold">فرم‌ها</h2>
        <div className="grid gap-4 sm:grid-cols-2">
          <Input label="نام و نام خانوادگی" placeholder="مثلاً سارا احمدی" />
          <Input label="شماره موبایل" placeholder="0912xxxxxxx" errorText="شماره موبایل معتبر نیست" />
          <Select
            label="رده سنی"
            placeholder="انتخاب کنید"
            options={[
              { value: "0-6", label: "۰ تا ۶ ماه" },
              { value: "6-12", label: "۶ تا ۱۲ ماه" },
              { value: "1-3", label: "۱ تا ۳ سال" },
            ]}
          />
          <Textarea label="توضیحات" placeholder="یادداشت شما..." />
          <div className="flex flex-col gap-2">
            <Checkbox label="عضویت در خبرنامه" defaultChecked />
            <Checkbox label="پذیرش قوانین" />
          </div>
          <div className="flex flex-col gap-2">
            <Radio name="shipping" label="ارسال اکسپرس" defaultChecked />
            <Radio name="shipping" label="ارسال عادی" />
          </div>
        </div>
      </section>

      <section className="flex flex-col gap-3">
        <h2 className="text-[var(--text-h2)] font-bold">Badge و Card</h2>
        <div className="flex flex-wrap gap-2">
          <Badge variant="brand">جدید</Badge>
          <Badge variant="accent">۲۰٪ تخفیف</Badge>
          <Badge variant="success">موجود</Badge>
          <Badge variant="warning">تنها ۲ عدد باقی‌مانده</Badge>
          <Badge variant="danger">ناموجود</Badge>
        </div>
        <Card className="max-w-sm">
          <CardHeader>عنوان کارت</CardHeader>
          <CardBody>محتوای نمونه برای بررسی فاصله‌گذاری و شعاع گوشه‌ها.</CardBody>
          <CardFooter>
            <Button size="sm">مشاهده</Button>
          </CardFooter>
        </Card>
      </section>

      <section className="flex flex-col gap-3">
        <h2 className="text-[var(--text-h2)] font-bold">Skeleton</h2>
        <div className="flex gap-3">
          <Skeleton className="h-24 w-24" />
          <div className="flex flex-1 flex-col gap-2">
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="h-4 w-1/2" />
            <Skeleton className="h-4 w-2/3" />
          </div>
        </div>
      </section>

      <section className="flex flex-col gap-3">
        <h2 className="text-[var(--text-h2)] font-bold">Tabs</h2>
        <Tabs
          items={[
            { key: "desc", label: "توضیحات", content: <p className="text-[var(--text-body-sm)]">محتوای برگه توضیحات.</p> },
            { key: "specs", label: "مشخصات", content: <p className="text-[var(--text-body-sm)]">محتوای برگه مشخصات.</p> },
            { key: "reviews", label: "نظرات", content: <p className="text-[var(--text-body-sm)]">محتوای برگه نظرات.</p> },
          ]}
        />
      </section>

      <section className="flex flex-col gap-3">
        <h2 className="text-[var(--text-h2)] font-bold">Pagination</h2>
        <Pagination currentPage={page} totalPages={9} onPageChange={setPage} />
      </section>

      <section className="flex flex-wrap gap-3">
        <h2 className="w-full text-[var(--text-h2)] font-bold">Modal / Drawer / Dropdown / Toast</h2>
        <Button onClick={() => setModalOpen(true)}>باز کردن Modal</Button>
        <Button variant="outline" onClick={() => setDrawerOpen(true)}>
          باز کردن Drawer
        </Button>
        <Dropdown
          trigger={<span className="rounded-[var(--radius-md)] border border-[var(--color-ink-300)] px-3 py-2 text-[var(--text-body-sm)]">منوی بازشو</span>}
          items={[
            { key: "edit", label: "ویرایش", onSelect: () => {} },
            { key: "delete", label: "حذف", onSelect: () => {}, danger: true },
          ]}
        />
        <Button
          variant="secondary"
          onClick={() =>
            pushToast({ variant: "success", title: "به سبد خرید اضافه شد", description: "کالسکه یویو بیبی زن" })
          }
        >
          نمایش Toast
        </Button>
      </section>

      <Modal open={modalOpen} onClose={() => setModalOpen(false)} title="تأیید عملیات" description="آیا از انجام این عملیات مطمئن هستید؟">
        <div className="flex justify-end gap-2">
          <Button variant="outline" onClick={() => setModalOpen(false)}>انصراف</Button>
          <Button onClick={() => setModalOpen(false)}>تأیید</Button>
        </div>
      </Modal>

      <Drawer open={drawerOpen} onClose={() => setDrawerOpen(false)} title="فیلترها" side="bottom">
        <p className="text-[var(--text-body-sm)] text-[var(--color-ink-600)]">نمونه محتوای Drawer.</p>
      </Drawer>
    </Container>
  );
}
