package edu.xmu.arch.hw8.service;

import edu.xmu.arch.hw8.web.dto.NotifyRequest;
import edu.xmu.arch.hw8.web.dto.NotifyResponse;

public interface NotificationService {

    NotifyResponse notify(NotifyRequest request);
}
