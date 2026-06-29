package com.middleware.lab04.ticket;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Modifying;
import org.springframework.transaction.annotation.Transactional;

import java.time.Instant;

public interface TicketRepository extends JpaRepository<Ticket, String> {

    @Modifying
    @Transactional
    int deleteByCreatedAtBefore(Instant cutoff);
}
