#ifndef __FS3399_GRF_H__
#define __FS3399_GRF_H__

//General Register Files (GRF)
#define GRF_BASE		0xFF770000
#define PMUGRF_BASE     0xFF320000

//IOMUX
#define    GRF_GPIO4A_IOMUX    		 (*(volatile unsigned int *)(GRF_BASE + 0xe020))
#define    GRF_GPIO4B_IOMUX    		 (*(volatile unsigned int *)(GRF_BASE + 0xe024))
#define    GRF_GPIO4C_IOMUX          (*(volatile unsigned int *)(GRF_BASE + 0xe028))

#define    PMUGRF_GPIO1B_IOMUX       (*(volatile unsigned int *)(PMUGRF_BASE + 0x0014))

#endif /*__FS3399_GRF_H__*/
