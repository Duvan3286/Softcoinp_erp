'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';

export default function SystemMaintenancePage() {
  const router = useRouter();

  useEffect(() => {
    router.replace('/system/maintenance/users');
  }, [router]);

  return null;
}
