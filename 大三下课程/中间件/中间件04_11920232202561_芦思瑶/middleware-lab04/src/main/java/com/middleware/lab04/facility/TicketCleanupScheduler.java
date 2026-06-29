package com.middleware.lab04.facility;

import com.middleware.lab04.config.LabProperties;
import com.middleware.lab04.ticket.TicketRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.scheduling.annotation.Scheduled;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

import java.time.Instant;
import java.time.temporal.ChronoUnit;

/**
 * 实验 Facility：定时任务（系统管理维度：数据生命周期）。
 */
@Component
public class TicketCleanupScheduler {

    private static final Logger log = LoggerFactory.getLogger(TicketCleanupScheduler.class);

    private final TicketRepository ticketRepository;
    private final LabProperties labProperties;

    public TicketCleanupScheduler(TicketRepository ticketRepository, LabProperties labProperties) {
        this.ticketRepository = ticketRepository;
        this.labProperties = labProperties;
    }

    @Scheduled(cron = "${lab.scheduled.cleanup-cron}")
    @Transactional
    public void purgeOldTickets() {
        int days = labProperties.scheduled().retentionDays();
        Instant cutoff = Instant.now().minus(days, ChronoUnit.DAYS);
        int removed = ticketRepository.deleteByCreatedAtBefore(cutoff);
        if (removed > 0) {
            log.info("Scheduled cleanup removed {} tickets older than {} days", removed, days);
        }
    }
}
