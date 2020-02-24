#include "qtest2.h"
#include "qtest2worker.h"
#include <QThread>
#include <QMap>
#include <QJSValue>
#include <QSharedPointer>
#include <QQmlEngine>
#include <QQmlContext>
#include "roc-lib/qroccommon.h"
using namespace Tests;
struct CallbackRequest
{
	QJSValue state;
	QJSValue callback;
};
class Tests::QTest2Private
{
public:
	QTest2Private() : currentRequestId(0) {}
	QThread workerThread;
	QTest2Worker* worker;
	QMap<int, QSharedPointer<CallbackRequest>> requests;
	int currentRequestId;
};
QTest2::QTest2(QObject* parent) : QObject(parent), d_priv(new Tests::QTest2Private())
{
	d_priv->worker = new QTest2Worker();
	d_priv->worker->moveToThread(&d_priv->workerThread);
	connect(&d_priv->workerThread, SIGNAL(finished()), d_priv->worker, SLOT(deleteLater()));
	d_priv->workerThread.start();
	connect(d_priv->worker, &QTest2Worker::testMethod2Done, this, &QTest2::testMethod2Handler);
	connect(d_priv->worker, &QTest2Worker::testMethodSync2Done, this, &QTest2::testMethodSync2Handler);
	connect(d_priv->worker, &QTest2Worker::testMethodWithNoResponse2Done, this, &QTest2::testMethodWithNoResponse2Handler);
	connect(d_priv->worker, &QTest2Worker::testMethodNoRequest2Done, this, &QTest2::testMethodNoRequest2Handler);
	connect(d_priv->worker, &QTest2Worker::testEvent2Raised, this, &QTest2::testEvent2);
	connect(d_priv->worker, &QTest2Worker::testEventComplex2Raised, this, &QTest2::testEventComplex2);
	connect(d_priv->worker, &QTest2Worker::testEventNoData2Raised, this, &QTest2::testEventNoData2);
	connect(d_priv->worker, &QTest2Worker::propString2Changed, this, &QTest2::propString2Changed);
	connect(d_priv->worker, &QTest2Worker::propComplex2Changed, this, &QTest2::propComplex2Changed);
}
QTest2::~QTest2()
{
	d_priv->workerThread.quit();
	d_priv->workerThread.wait();
}
void QTest2::testMethod2(QJsonValue val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testMethod2(val, requestId);
}
void QTest2::testMethodSync2(QJsonValue val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testMethodSync2(val, requestId);
}
void QTest2::testMethodWithNoResponse2(QJsonValue val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testMethodWithNoResponse2(val, requestId);
}
void QTest2::testMethodNoRequest2(QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testMethodNoRequest2(requestId);
}
void QTest2::testMethod2Handler(QJsonValue val, int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		auto engine = QQmlEngine::contextForObject(this)->engine();
		QJSValue e = engine->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", convertJsonValueToJsValue(engine, val));
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTest2::testMethodSync2Handler(QJsonValue val, int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		auto engine = QQmlEngine::contextForObject(this)->engine();
		QJSValue e = engine->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", convertJsonValueToJsValue(engine, val));
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTest2::testMethodWithNoResponse2Handler(int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		QJSValue e = QQmlEngine::contextForObject(this)->engine()->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", QJSValue::NullValue);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTest2::testMethodNoRequest2Handler(int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		QJSValue e = QQmlEngine::contextForObject(this)->engine()->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", QJSValue::NullValue);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
QVariant QTest2::getPropString2()
{
	return d_priv->worker->getPropString2();
}
void QTest2::setPropString2(QVariant val)
{
	d_priv->worker->setPropString2(val);
}
QJsonValue QTest2::getPropComplex2()
{
	return d_priv->worker->getPropComplex2();
}
void QTest2::setPropComplex2(QJsonValue val)
{
	d_priv->worker->setPropComplex2(val);
}
