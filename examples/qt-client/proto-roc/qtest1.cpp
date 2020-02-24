#include "qtest1.h"
#include "qtest1worker.h"
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
class Tests::QTest1Private
{
public:
	QTest1Private() : currentRequestId(0) {}
	QThread workerThread;
	QTest1Worker* worker;
	QMap<int, QSharedPointer<CallbackRequest>> requests;
	int currentRequestId;
};
QTest1::QTest1(QObject* parent) : QObject(parent), d_priv(new Tests::QTest1Private())
{
	d_priv->worker = new QTest1Worker();
	d_priv->worker->moveToThread(&d_priv->workerThread);
	connect(&d_priv->workerThread, SIGNAL(finished()), d_priv->worker, SLOT(deleteLater()));
	d_priv->workerThread.start();
	connect(d_priv->worker, &QTest1Worker::testMethodDone, this, &QTest1::testMethodHandler);
	connect(d_priv->worker, &QTest1Worker::testMethodSyncDone, this, &QTest1::testMethodSyncHandler);
	connect(d_priv->worker, &QTest1Worker::testMethodWithNoResponseDone, this, &QTest1::testMethodWithNoResponseHandler);
	connect(d_priv->worker, &QTest1Worker::testMethodPrimitiveDone, this, &QTest1::testMethodPrimitiveHandler);
	connect(d_priv->worker, &QTest1Worker::testMethodNoRequestOrResponseDone, this, &QTest1::testMethodNoRequestOrResponseHandler);
	connect(d_priv->worker, &QTest1Worker::testMethodNoRequestDone, this, &QTest1::testMethodNoRequestHandler);
	connect(d_priv->worker, &QTest1Worker::testMethodNoResponseDone, this, &QTest1::testMethodNoResponseHandler);
	connect(d_priv->worker, &QTest1Worker::testEventRaised, this, &QTest1::testEvent);
	connect(d_priv->worker, &QTest1Worker::testEventComplexRaised, this, &QTest1::testEventComplex);
	connect(d_priv->worker, &QTest1Worker::testEventNoDataRaised, this, &QTest1::testEventNoData);
	connect(d_priv->worker, &QTest1Worker::propStringChanged, this, &QTest1::propStringChanged);
	connect(d_priv->worker, &QTest1Worker::propComplexChanged, this, &QTest1::propComplexChanged);
}
QTest1::~QTest1()
{
	d_priv->workerThread.quit();
	d_priv->workerThread.wait();
}
void QTest1::testMethod(QJsonValue val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testMethod(val, requestId);
}
void QTest1::testMethodSync(QJsonValue val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testMethodSync(val, requestId);
}
void QTest1::testMethodWithNoResponse(QJsonValue val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testMethodWithNoResponse(val, requestId);
}
void QTest1::testMethodPrimitive(bool val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testMethodPrimitive(val, requestId);
}
void QTest1::testMethodNoRequestOrResponse(QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testMethodNoRequestOrResponse(requestId);
}
void QTest1::testMethodNoRequest(QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testMethodNoRequest(requestId);
}
void QTest1::testMethodNoResponse(bool val, QJSValue state, QJSValue callback)
{
	auto requestId = d_priv->currentRequestId++;
	d_priv->requests.insert(requestId, QSharedPointer<CallbackRequest>(new CallbackRequest { state, callback }));
	d_priv->worker->testMethodNoResponse(val, requestId);
}
void QTest1::testMethodHandler(QJsonValue val, int requestId, QString error)
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
void QTest1::testMethodSyncHandler(QJsonValue val, int requestId, QString error)
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
void QTest1::testMethodWithNoResponseHandler(int requestId, QString error)
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
void QTest1::testMethodPrimitiveHandler(bool val, int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		auto engine = QQmlEngine::contextForObject(this)->engine();
		QJSValue e = engine->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", val);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTest1::testMethodNoRequestOrResponseHandler(int requestId, QString error)
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
void QTest1::testMethodNoRequestHandler(bool val, int requestId, QString error)
{
	if(!d_priv->requests.contains(requestId)) { qCritical("Couldn't find the given request id."); return; }
	auto request = d_priv->requests.value(requestId);
	d_priv->requests.remove(requestId);
	if(request->callback.isCallable())
	{
		auto engine = QQmlEngine::contextForObject(this)->engine();
		QJSValue e = engine->newObject();
		e.setProperty("state", request->state);
		e.setProperty("result", val);
		e.setProperty("error", error);
		QJSValueList args;
		args.push_back(e);
		request->callback.call(args);
	}
}
void QTest1::testMethodNoResponseHandler(int requestId, QString error)
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
QVariant QTest1::getPropString()
{
	return d_priv->worker->getPropString();
}
void QTest1::setPropString(QVariant val)
{
	d_priv->worker->setPropString(val);
}
QJsonValue QTest1::getPropComplex()
{
	return d_priv->worker->getPropComplex();
}
void QTest1::setPropComplex(QJsonValue val)
{
	d_priv->worker->setPropComplex(val);
}
