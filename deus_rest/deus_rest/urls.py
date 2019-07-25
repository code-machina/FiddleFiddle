"""deus_rest URL Configuration

The `urlpatterns` list routes URLs to views. For more information please see:
    https://docs.djangoproject.com/en/2.0/topics/http/urls/
Examples:
Function views
    1. Add an import:  from my_app import views
    2. Add a URL to urlpatterns:  path('', views.home, name='home')
Class-based views
    1. Add an import:  from other_app.views import Home
    2. Add a URL to urlpatterns:  path('', Home.as_view(), name='home')
Including another URLconf
    1. Import the include() function: from django.urls import include, path
    2. Add a URL to urlpatterns:  path('blog/', include('blog.urls'))
"""
from django.contrib import admin
from django.urls import path
from django.conf.urls import url, include
from django.views import generic

from rest_framework.schemas import get_schema_view
from rest_framework_simplejwt.views import (
    TokenObtainPairView,
    TokenRefreshView,
)
from rest_framework.routers import DefaultRouter
from machina import views
from machina.views import TestViewSet, UniqueUrlViewSet

router = DefaultRouter()

router.register(r'tests', views.TestViewSet)
router.register(r'uurls', views.UniqueUrlViewSet)

test_list = TestViewSet.as_view({
    'get':'list',
    'post':'create'
})

test_detail = TestViewSet.as_view({
    'get': 'retrieve',
    'put': 'update',
    'patch': 'partial_update',
    'delete': 'destroy'
})

uurl_list = UniqueUrlViewSet.as_view({
    'get': 'list',
    'post': 'create'
})

uurl_detail = UniqueUrlViewSet.as_view({
    'get': 'retrieve',
    'put': 'update',
    'patch': 'partial_update',
    'delete': 'destroy'
})

urlpatterns = [
    path('admin/', admin.site.urls),
]

urlpatterns += [
    url(r'^$', generic.RedirectView.as_view(
         url='/api/', permanent=False)),
    url(r'^api/$', get_schema_view()),
    url(r'^api/auth/', include(
        'rest_framework.urls', namespace='rest_framework')),
    url(r'^api/auth/token/obtain/$', TokenObtainPairView.as_view()),
    url(r'^api/auth/token/refresh/$', TokenRefreshView.as_view()),
]

urlpatterns += [
    url(r'^tests/$', test_list, name='test-list'),
    url(r'^tests/(?P<pk>[0-9]+)/$', test_detail, name='test-detail')
]

urlpatterns += [
    url(r'^uurls/$', uurl_list, name='uurl-list'),
    url(r'^uurls/(?P<pk>[0-9]+)/$', uurl_detail, name='uurl-detail')
]