package com.middleware.lab04.integration;

import com.middleware.lab04.config.LabProperties;
import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import org.springframework.core.annotation.Order;
import org.springframework.http.HttpHeaders;
import org.springframework.lang.NonNull;
import org.springframework.stereotype.Component;
import org.springframework.web.filter.OncePerRequestFilter;

import java.io.IOException;

/**
 * 保护 /api/inbound/**；本地调试可设置 lab.webhook.auth-disabled=true。
 */
@Component
@Order(0)
public class WebhookBearerAuthFilter extends OncePerRequestFilter {

    private final LabProperties labProperties;

    public WebhookBearerAuthFilter(LabProperties labProperties) {
        this.labProperties = labProperties;
    }

    @Override
    protected void doFilterInternal(
            @NonNull HttpServletRequest request,
            @NonNull HttpServletResponse response,
            @NonNull FilterChain filterChain
    ) throws ServletException, IOException {
        String path = request.getRequestURI();
        if (!path.startsWith(request.getContextPath() + "/api/inbound")) {
            filterChain.doFilter(request, response);
            return;
        }
        if (labProperties.webhook().authDisabled()) {
            filterChain.doFilter(request, response);
            return;
        }
        String auth = request.getHeader(HttpHeaders.AUTHORIZATION);
        String expected = "Bearer " + labProperties.webhook().secret();
        if (auth != null && auth.equals(expected)) {
            filterChain.doFilter(request, response);
            return;
        }
        response.setStatus(HttpServletResponse.SC_UNAUTHORIZED);
        response.setContentType("application/json;charset=UTF-8");
        response.getWriter().write("{\"error\":\"unauthorized\"}");
    }
}
